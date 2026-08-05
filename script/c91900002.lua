-- Smalls, Guardian of the Wetlands
local s,id=GetID()
function s.initial_effect(c)
  c:EnableReviveLimit()
  
  -- Must first be Special Summoned from the hand
  local e1=Effect.CreateEffect(c)
  e1:SetType(EFFECT_TYPE_FIELD)
  e1:SetCode(EFFECT_SPSUMMON_PROC)
  e1:SetProperty(EFFECT_FLAG_UNCOPYABLE)
  e1:SetRange(LOCATION_HAND)
  e1:SetCondition(s.spcon)
  e1:SetTarget(s.sptg)
  e1:SetOperation(s.spop)
  c:RegisterEffect(e1)

  -- Reveal, shuffle into Deck, add Wetlands
  local e2=Effect.CreateEffect(c)
  e2:SetDescription(aux.Stringid(id,0))
  e2:SetCategory(CATEGORY_TODECK+CATEGORY_TOHAND+CATEGORY_SEARCH)
  e2:SetType(EFFECT_TYPE_IGNITION)
  e2:SetRange(LOCATION_HAND)
  e2:SetCost(s.thcost)
  e2:SetTarget(s.thtg)
  e2:SetOperation(s.thop)
  c:RegisterEffect(e2)

  -- Unaffected by opponent's card effects
  local e3=Effect.CreateEffect(c)
  e3:SetType(EFFECT_TYPE_SINGLE)
  e3:SetProperty(EFFECT_FLAG_SINGLE_RANGE)
  e3:SetRange(LOCATION_MZONE)
  e3:SetCode(EFFECT_IMMUNE_EFFECT)
  e3:SetCondition(s.immcon)
  e3:SetValue(s.efilter)
  c:RegisterEffect(e3)

  -- Quick Effect: Change Level and Type
  local e4=Effect.CreateEffect(c)
  e4:SetDescription(aux.Stringid(id,1))
  e4:SetType(EFFECT_TYPE_QUICK_O)
  e4:SetCode(EVENT_FREE_CHAIN)
  e4:SetRange(LOCATION_MZONE)
  e4:SetProperty(EFFECT_FLAG_CARD_TARGET)
  e4:SetCountLimit(1)
  e4:SetTarget(s.lvtg)
  e4:SetOperation(s.lvop)
  c:RegisterEffect(e4)
end

-- /// Effect 1: Special Summon Procedure ///
function s.spfilter1(c)
  return c:IsFaceup() and c:IsCode(91900001) and c:IsAbleToDeckAsCost()
end
function s.spfilter2(c)
  return c:IsFaceup() and c:IsRace(RACE_AQUA) and c:IsAbleToDeckAsCost()
end
function s.spcon(e,c)
  if c==nil then return true end
  local tp=c:GetControler()
  local rg=Duel.GetMatchingGroup(Card.IsFaceup,tp,LOCATION_REMOVED,0,nil)
  local g1=rg:Filter(s.spfilter1,nil)
  local g2=rg:Filter(s.spfilter2,nil)
  -- Ensure there are at least two distinct valid cards
  for tc1 in aux.Next(g1) do
    for tc2 in aux.Next(g2) do
      if tc1~=tc2 then return true end
    end
  end
  return false
end
function s.sptg(e,tp,eg,ep,ev,re,r,rp,chk,c)
  local rg=Duel.GetMatchingGroup(Card.IsFaceup,tp,LOCATION_REMOVED,0,nil)
  local g1=rg:Filter(s.spfilter1,nil)
  local g2=rg:Filter(s.spfilter2,nil)
  
  Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_TODECK)
  local sg1=g1:Select(tp,1,1,nil)
  local tc1=sg1:GetFirst()
  g2:RemoveCard(tc1) -- Prevent selecting the same card twice if Smalls is Aqua
  
  Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_TODECK)
  local sg2=g2:Select(tp,1,1,nil)
  sg1:Merge(sg2)
  
  if #sg1==2 then
    sg1:KeepAlive()
    e:SetLabelObject(sg1)
    return true
  end
  return false
end
function s.spop(e,tp,eg,ep,ev,re,r,rp,c)
  local g=e:GetLabelObject()
  Duel.SendtoDeck(g,nil,SEQ_DECKSHUFFLE,REASON_COST)
  g:DeleteGroup()
end

-- /// Effect 2: Reveal, Shuffle, Add Wetlands ///
function s.thcost(e,tp,eg,ep,ev,re,r,rp,chk)
  local c=e:GetHandler()
  if chk==0 then return not c:IsPublic() end
  Duel.ConfirmCards(1-tp,c)
end
function s.thfilter(c)
  return c:IsCode(2084239) and c:IsAbleToHand() -- 2084239 is Wetlands
end
function s.thtg(e,tp,eg,ep,ev,re,r,rp,chk)
  if chk==0 then return e:GetHandler():IsAbleToDeck()
    and Duel.IsExistingMatchingCard(s.thfilter,tp,LOCATION_DECK+LOCATION_GRAVE,0,1,nil) end
  Duel.SetOperationInfo(0,CATEGORY_TODECK,e:GetHandler(),1,0,0)
  Duel.SetOperationInfo(0,CATEGORY_TOHAND,nil,1,tp,LOCATION_DECK+LOCATION_GRAVE)
end
function s.thop(e,tp,eg,ep,ev,re,r,rp)
  local c=e:GetHandler()
  -- Check if card is still in hand and successfully shuffles into Deck
  if c:IsRelateToEffect(e) and Duel.SendtoDeck(c,nil,SEQ_DECKSHUFFLE,REASON_EFFECT)>0 and c:IsLocation(LOCATION_DECK) then
    Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_ATOHAND)
    local g=Duel.SelectMatchingCard(tp,aux.NecroValleyFilter(s.thfilter),tp,LOCATION_DECK+LOCATION_GRAVE,0,1,1,nil)
    if #g>0 then
      Duel.SendtoHand(g,nil,REASON_EFFECT)
      Duel.ConfirmCards(1-tp,g)
    end
  end
end

-- /// Effect 3: Immunity ///
function s.envfilter(c)
  return c:IsFaceup() and c:IsCode(2084239) -- Wetlands
end
function s.immcon(e)
  return Duel.IsExistingMatchingCard(s.envfilter,0,LOCATION_ONFIELD,LOCATION_ONFIELD,1,nil)
end
function s.efilter(e,te)
  -- Immune to effects owned by a different player
  return te:GetOwnerPlayer()~=e:GetHandlerPlayer()
end

-- /// Effect 4: Quick Effect Type/Level Change ///
function s.lvtg(e,tp,eg,ep,ev,re,r,rp,chk,chkc)
  if chkc then return chkc:IsLocation(LOCATION_MZONE) and chkc:IsFaceup() end
  if chk==0 then return Duel.IsExistingTarget(Card.IsFaceup,tp,LOCATION_MZONE,LOCATION_MZONE,1,nil) end
  Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_TARGET)
  Duel.SelectTarget(tp,Card.IsFaceup,tp,LOCATION_MZONE,LOCATION_MZONE,1,1,nil)
end
function s.lvop(e,tp,eg,ep,ev,re,r,rp)
  local tc=Duel.GetFirstTarget()
  if tc:IsRelateToEffect(e) and tc:IsFaceup() then
    local c=e:GetHandler()
    
    -- Level Becomes 2
    local e1=Effect.CreateEffect(c)
    e1:SetType(EFFECT_TYPE_SINGLE)
    e1:SetCode(EFFECT_CHANGE_LEVEL)
    e1:SetValue(2)
    e1:SetReset(RESET_EVENT+RESETS_STANDARD+RESET_PHASE+PHASE_END)
    tc:RegisterEffect(e1)
    
    -- Type Becomes Aqua
    local e2=Effect.CreateEffect(c)
    e2:SetType(EFFECT_TYPE_SINGLE)
    e2:SetCode(EFFECT_CHANGE_RACE)
    e2:SetValue(RACE_AQUA)
    e2:SetReset(RESET_EVENT+RESETS_STANDARD+RESET_PHASE+PHASE_END)
    tc:RegisterEffect(e2)
  end
end