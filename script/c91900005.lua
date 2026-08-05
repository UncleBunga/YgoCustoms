-- Chaos Kuriboh
local s,id=GetID()
function s.initial_effect(c)
	-- Quick Effect: Banish from hand/field for protection
	local e1=Effect.CreateEffect(c)
	e1:SetDescription(aux.Stringid(id,0))
	e1:SetType(EFFECT_TYPE_QUICK_O)
	e1:SetCode(EVENT_FREE_CHAIN)
	e1:SetRange(LOCATION_HAND+LOCATION_MZONE)
	e1:SetHintTiming(0,TIMINGS_CHECK_MONSTER+TIMING_MAIN_END)
	e1:SetCountLimit(1,id)
	e1:SetCost(s.cost)
	e1:SetOperation(s.op)
	c:RegisterEffect(e1)
end

-- /// Effect 1: Cost & Operation ///
function s.cost(e,tp,eg,ep,ev,re,r,rp,chk)
	local c=e:GetHandler()
	if chk==0 then return c:IsAbleToRemoveAsCost() end
	Duel.Remove(c,POS_FACEUP,REASON_COST)
end

function s.op(e,tp,eg,ep,ev,re,r,rp)
	local c=e:GetHandler()
	-- Apply protection to LIGHT and DARK monsters you control for the rest of the turn
	local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_IMMUNE_EFFECT)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(s.immtg)
	e1:SetValue(s.efilter)
	e1:SetReset(RESET_PHASE+PHASE_END)
	Duel.RegisterEffect(e1,tp)

	-- Register a flag to track that the effect resolved successfully
	Duel.RegisterFlagEffect(tp,id,RESET_PHASE+PHASE_END,0,1)

	-- Register End Phase Trigger
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_CONTINUOUS)
	e2:SetCode(EVENT_PHASE+PHASE_END)
	e2:SetCountLimit(1)
	e2:SetCondition(s.epcon)
	e2:SetOperation(s.epop)
	e2:SetReset(RESET_PHASE+PHASE_END)
	Duel.RegisterEffect(e2,tp)
end

function s.immtg(e,c)
	return c:IsAttribute(ATTRIBUTE_LIGHT+ATTRIBUTE_DARK)
end

function s.efilter(e,te)
	return te:GetOwnerPlayer()~=e:GetHandlerPlayer()
end

-- /// End Phase Token Menu Logic ///
function s.epcon(e,tp,eg,ep,ev,re,r,rp)
	return Duel.GetFlagEffect(tp,id)>0
end

function s.epop(e,tp,eg,ep,ev,re,r,rp)
	local b1=Duel.GetLocationCount(tp,LOCATION_MZONE)>0 and Duel.IsPlayerCanSpecialSummonMonster(tp,30327675,0,TYPES_TOKEN,1000,500,2,RACE_FIEND,ATTRIBUTE_DARK)
	local b2=Duel.GetLocationCount(tp,LOCATION_MZONE)>0 and Duel.IsPlayerCanSpecialSummonMonster(tp,30327676,0,TYPES_TOKEN+TYPE_TUNER,500,1000,2,RACE_FAIRY,ATTRIBUTE_LIGHT)

	if not (b1 or b2) then return end

	if Duel.SelectYesNo(tp,aux.Stringid(id,1)) then
		local op=Duel.SelectEffect(tp,
			{b1, aux.Stringid(id,2)},
			{b2, aux.Stringid(id,3)}
		)

		if op==1 then
			local token=Duel.CreateToken(tp,30327675)
			Duel.SpecialSummon(token,0,tp,tp,false,false,POS_FACEUP)
		elseif op==2 then
			local token=Duel.CreateToken(tp,30327676)
			Duel.SpecialSummon(token,0,tp,tp,false,false,POS_FACEUP)
		end
	end
end