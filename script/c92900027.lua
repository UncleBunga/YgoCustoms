-- Slasher's Doll Store
local s,id=GetID()
function s.initial_effect(c)
    -- Activate: Add Level 2 Slasher, then optionally Summon or Set it
    local e1=Effect.CreateEffect(c)
    e1:SetCategory(CATEGORY_TOHAND+CATEGORY_SEARCH+CATEGORY_SPECIAL_SUMMON)
    e1:SetType(EFFECT_TYPE_ACTIVATE)
    e1:SetCode(EVENT_FREE_CHAIN)
    e1:SetCountLimit(1,id,EFFECT_COUNT_CODE_OATH)
    e1:SetTarget(s.target)
    e1:SetOperation(s.activate)
    c:RegisterEffect(e1)
end

-- Filter for Level 2 "Slasher" monster
function s.thfilter(c,e,tp)
    return c:IsSetCard(0x991) and c:IsLevel(2) and c:IsAbleToHand()
end

function s.target(e,tp,eg,ep,ev,re,r,rp,chk)
    if chk==0 then return Duel.IsExistingMatchingCard(s.thfilter,tp,LOCATION_DECK,0,1,nil) end
    Duel.SetOperationInfo(0,CATEGORY_TOHAND,nil,1,tp,LOCATION_DECK)
end

function s.activate(e,tp,eg,ep,ev,re,r,rp)
    Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_ATOHAND)
    local g=Duel.SelectMatchingCard(tp,s.thfilter,tp,LOCATION_DECK,0,1,1,nil)
    if #g>0 then
        local tc=g:GetFirst()
        if Duel.SendtoHand(tc,nil,REASON_EFFECT)>0 and tc:IsLocation(LOCATION_HAND) then
            Duel.ConfirmCards(1-tp,tc)
            
            -- Check if the player wants to and can Special Summon (Face-up Attack) or Set (Facedown Defense)
            -- Ritual monsters like Chucky will safely stay in hand because they cannot be arbitrarily Special Summoned or Set like this unless conditions are met.
            local b1 = tc:IsCanBeSpecialSummoned(e,0,tp,false,false,POS_FACEUP_ATTACK) and Duel.GetLocationCount(tp,LOCATION_MZONE)>0
            local b2 = tc:IsMSetable(true,nil) and Duel.GetLocationCount(tp,LOCATION_MZONE)>0
            
            if (b1 or b2) and (tc:IsType(TYPE_RITUAL) == false) then -- Explicitly bypasses if it's a Ritual to let Chucky stay in hand smoothly
                local ops = {}
                local opval = {}
                local idx = 0
                
                if b1 then
                    ops[idx+1] = aux.Stringid(id,0) -- "Special Summon in Attack Position"
                    opval[idx] = 1
                    idx = idx + 1
                end
                if b2 then
                    ops[idx+1] = aux.Stringid(id,1) -- "Set in Defense Position"
                    opval[idx] = 2
                    idx = idx + 1
                end
                
                ops[idx+1] = aux.Stringid(id,2) -- "Do nothing (Keep in Hand)"
                opval[idx] = 0
                
                local op = Duel.SelectOption(tp,table.unpack(ops))
                local choice = opval[op]
                
                if choice == 1 then
                    Duel.SpecialSummon(tc,0,tp,tp,false,false,POS_FACEUP_ATTACK)
                elseif choice == 2 then
                    Duel.MSet(tp,tc,true,nil)
                end
            end
        end
    end
end